use encoding_rs::Encoding;

/// Read and parse desktop.ini content from a folder.
/// Returns the file content as UTF-8 String, or None if file doesn't exist.
pub fn read_desktop_ini(folder_path: &str) -> Option<String> {
    let ini_path = std::path::Path::new(folder_path).join("desktop.ini");
    if !ini_path.exists() {
        return None;
    }

    let bytes = std::fs::read(&ini_path).ok()?;
    Some(decode_desktop_ini_bytes(&bytes))
}

/// Decode desktop.ini bytes into a UTF-8 String.
///
/// Encoding detection strategy (matches Windows Explorer behavior):
///   1. BOM 0xFF 0xFE → UTF-16 LE
///   2. No BOM        → system ANSI code page (GBK on Chinese Windows)
///
/// Windows INI APIs (GetPrivateProfileStringW) only recognize these two
/// encodings. UTF-8 is NOT supported by the Windows shell for desktop.ini.
fn decode_desktop_ini_bytes(bytes: &[u8]) -> String {
    // Detect UTF-16 LE with BOM
    if bytes.len() >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE {
        let utf16: Vec<u16> = bytes[2..]
            .chunks_exact(2)
            .map(|chunk| u16::from_le_bytes([chunk[0], chunk[1]]))
            .collect();
        return String::from_utf16(&utf16).unwrap_or_default();
    }

    // No BOM — treat as system ANSI code page.
    // On Chinese Windows this is GBK (CP936); on Japanese it's Shift_JIS (CP932);
    // on Western systems it's windows-1252.
    // We try the most common Chinese encoding first, then fall back to UTF-8.
    if let Some(gbk) = Encoding::for_label(b"gbk") {
        let (decoded, _, had_errors) = gbk.decode(bytes);
        if !had_errors {
            return decoded.into_owned();
        }
    }

    // Fallback: if GBK decoding produced errors, try UTF-8
    String::from_utf8(bytes.to_vec()).unwrap_or_else(|_| {
        // Last resort: decode as windows-1252 (never fails, maps every byte)
        if let Some(win1252) = Encoding::for_label(b"windows-1252") {
            let (decoded, _, _) = win1252.decode(bytes);
            decoded.into_owned()
        } else {
            String::new()
        }
    })
}

/// Write desktop.ini content to a folder.
/// Ensures proper encoding (UTF-16 LE with BOM) for Windows compatibility.
pub fn write_desktop_ini(folder_path: &str, content: &str) -> std::io::Result<()> {
    let ini_path = std::path::Path::new(folder_path).join("desktop.ini");

    // Encode as UTF-16 LE with BOM
    let mut bytes: Vec<u8> = vec![0xFF, 0xFE]; // BOM
    for code_unit in content.encode_utf16() {
        bytes.extend_from_slice(&code_unit.to_le_bytes());
    }

    std::fs::write(&ini_path, &bytes)?;
    Ok(())
}

/// Parse desktop.ini content and return the IconResource value from the
/// [.ShellClassInfo] section only.
///
/// Returns (icon_resource_value, all_other_content).
/// IconResource lines in OTHER sections are preserved (kept in other_lines).
pub fn parse_icon_resource(content: &str) -> (Option<String>, Vec<String>) {
    let mut icon_resource: Option<String> = None;
    let mut other_lines: Vec<String> = Vec::new();
    let mut current_section: Option<String> = None;

    for line in content.lines() {
        let trimmed = line.trim();

        // Track section headers
        if trimmed.starts_with('[') && trimmed.ends_with(']') {
            current_section = Some(trimmed.to_lowercase());
            other_lines.push(line.to_string());
            continue;
        }

        // Only strip IconResource when inside [.ShellClassInfo]
        let in_shell_class_info =
            current_section.as_deref() == Some("[.shellclassinfo]");
        let trimmed_lower = trimmed.to_lowercase();

        if in_shell_class_info && trimmed_lower.starts_with("iconresource=") {
            // Extract the value after '='
            if let Some(value) = trimmed.splitn(2, '=').nth(1) {
                icon_resource = Some(value.trim().to_string());
            }
            // Do NOT push this line to other_lines — it's being replaced/removed
        } else {
            other_lines.push(line.to_string());
        }
    }

    (icon_resource, other_lines)
}

/// Build new desktop.ini content by setting/updating IconResource in
/// [.ShellClassInfo]. Preserves all existing non-IconResource lines and
/// all content in other sections.
pub fn build_content_with_icon(
    existing_content: Option<&str>,
    ico_path: &str,
) -> String {
    let icon_line = format!("IconResource={},0", ico_path);

    let (_existing_icon, other_lines) = match existing_content {
        Some(content) => parse_icon_resource(content),
        None => (None, Vec::new()),
    };

    // Rebuild content, inserting IconResource right after [.ShellClassInfo]
    let mut result: Vec<String> = Vec::new();

    for line in &other_lines {
        let trimmed = line.trim().to_lowercase();

        if trimmed == "[.shellclassinfo]" {
            result.push(line.clone());
            result.push(icon_line.clone());
        } else {
            result.push(line.clone());
        }
    }

    // If no [.ShellClassInfo] section existed, append one with IconResource
    if !other_lines
        .iter()
        .any(|l| l.trim().to_lowercase() == "[.shellclassinfo]")
    {
        if !result.is_empty() && !result.last().unwrap().is_empty() {
            result.push(String::new());
        }
        result.push("[.ShellClassInfo]".to_string());
        result.push(icon_line);
    }

    result.join("\r\n")
}

/// Build content after removing IconResource from [.ShellClassInfo] (for restore
/// default). Returns None if unchanged, Some(new_content) if modified.
pub fn build_content_without_icon(existing_content: &str) -> Option<String> {
    let (icon_resource, other_lines) = parse_icon_resource(existing_content);

    // If there was no IconResource in [.ShellClassInfo], nothing to do
    if icon_resource.is_none() {
        return None;
    }

    let content = other_lines.join("\r\n");
    // Trim trailing empty lines but keep the file
    let content = content
        .trim_end_matches(|c| c == '\r' || c == '\n')
        .to_string();

    Some(content)
}

#[cfg(test)]
mod tests {
    use super::*;

    // =========================================================================
    // Encoding tests
    // =========================================================================

    #[test]
    fn test_decode_gbk_desktop_ini() {
        // Simulate a GBK-encoded desktop.ini with Chinese content
        let gbk = Encoding::for_label(b"gbk").unwrap();
        let original =
            "[.ShellClassInfo]\r\nIconResource=D:\\图标\\蓝色.ico,0\r\nInfoTip=测试文件夹";
        let (encoded, _, _) = gbk.encode(original);

        let decoded = decode_desktop_ini_bytes(&encoded);
        assert!(decoded.contains("[.ShellClassInfo]"));
        assert!(decoded.contains("IconResource=D:\\图标\\蓝色.ico,0"));
        assert!(decoded.contains("InfoTip=测试文件夹"));
    }

    #[test]
    fn test_decode_utf16_le_with_bom() {
        // Build UTF-16 LE with BOM bytes manually
        let content = "[.ShellClassInfo]\r\nIconResource=C:\\test.ico,0\r\n";
        let utf16: Vec<u16> = content.encode_utf16().collect();
        let mut bytes = vec![0xFF, 0xFE]; // BOM
        for code_unit in &utf16 {
            bytes.extend_from_slice(&code_unit.to_le_bytes());
        }

        let decoded = decode_desktop_ini_bytes(&bytes);
        assert!(decoded.contains("[.ShellClassInfo]"));
        assert!(decoded.contains("IconResource=C:\\test.ico,0"));
    }

    #[test]
    fn test_decode_plain_ascii_no_bom() {
        // Plain ASCII (subset of both UTF-8 and GBK) — should decode correctly
        let content = b"[.ShellClassInfo]\r\nIconResource=C:\\test.ico,0\r\n";
        let decoded = decode_desktop_ini_bytes(content);
        assert!(decoded.contains("[.ShellClassInfo]"));
        assert!(decoded.contains("IconResource=C:\\test.ico,0"));
    }

    #[test]
    fn test_read_write_roundtrip_utf16() {
        // Write with write_desktop_ini, read back, verify
        let tmpdir = std::env::temp_dir().join("fim_test_roundtrip");
        std::fs::create_dir_all(&tmpdir).unwrap();

        let content = "[.ShellClassInfo]\r\nIconResource=D:\\test.ico,0\r\nInfoTip=Hello";
        write_desktop_ini(tmpdir.to_str().unwrap(), content).unwrap();

        let read_back = read_desktop_ini(tmpdir.to_str().unwrap()).unwrap();
        assert_eq!(read_back, content);

        // Cleanup
        let _ = std::fs::remove_dir_all(&tmpdir);
    }

    #[test]
    fn test_read_write_roundtrip_chinese() {
        // Chinese content roundtrip via temp file
        let tmpdir = std::env::temp_dir().join("fim_test_roundtrip_cn");
        std::fs::create_dir_all(&tmpdir).unwrap();

        let content = "[.ShellClassInfo]\r\nIconResource=D:\\图标\\蓝色.ico,0\r\nInfoTip=测试";
        write_desktop_ini(tmpdir.to_str().unwrap(), content).unwrap();

        let read_back = read_desktop_ini(tmpdir.to_str().unwrap()).unwrap();
        assert_eq!(read_back, content);

        let _ = std::fs::remove_dir_all(&tmpdir);
    }

    // =========================================================================
    // Section-aware parse_icon_resource tests
    // =========================================================================

    #[test]
    fn test_parse_icon_resource_present() {
        let content =
            "[.ShellClassInfo]\r\nIconResource=D:\\Icons\\test.ico,0\r\nInfoTip=Test";
        let (icon, other) = parse_icon_resource(content);
        assert_eq!(icon, Some("D:\\Icons\\test.ico,0".to_string()));
        assert!(other.iter().any(|l| l.contains("InfoTip=Test")));
        // IconResource should NOT be in other_lines
        assert!(!other.iter().any(|l| l.contains("IconResource=")));
    }

    #[test]
    fn test_parse_icon_resource_absent() {
        let content = "[.ShellClassInfo]\r\nInfoTip=Test";
        let (icon, _) = parse_icon_resource(content);
        assert_eq!(icon, None);
    }

    #[test]
    fn test_parse_icon_resource_preserves_other_section() {
        // IconResource in [OtherSection] must NOT be removed
        let content = concat!(
            "[.ShellClassInfo]\r\nIconResource=shell.ico,0\r\n",
            "[OtherSection]\r\nIconResource=other.ico,0\r\n",
        );
        let (icon, other) = parse_icon_resource(content);
        // Should only capture the ShellClassInfo icon
        assert_eq!(icon, Some("shell.ico,0".to_string()));
        // other.ico in OtherSection must be preserved
        assert!(
            other.iter().any(|l| l.trim() == "IconResource=other.ico,0"),
            "IconResource in OtherSection must NOT be removed"
        );
        // shell.ico must NOT be in other_lines
        assert!(!other.iter().any(|l| l.contains("shell.ico")));
    }

    #[test]
    fn test_parse_icon_resource_other_section_only() {
        // IconResource only exists in non-ShellClassInfo sections
        let content = "[OtherSection]\r\nIconResource=other.ico,0\r\n";
        let (icon, other) = parse_icon_resource(content);
        assert_eq!(icon, None, "Should not pick up IconResource from non-ShellClassInfo sections");
        assert!(other.iter().any(|l| l.contains("IconResource=other.ico,0")));
    }

    #[test]
    fn test_parse_icon_resource_multiple_sections() {
        // Complex file with multiple sections
        let content = concat!(
            "[.ShellClassInfo]\r\n",
            "IconResource=myicon.ico,0\r\n",
            "InfoTip=My Folder\r\n",
            "\r\n",
            "[ViewState]\r\n",
            "Mode=\r\n",
            "Vid=\r\n",
            "\r\n",
            "[CustomSection]\r\n",
            "IconResource=leave_me_alone.ico,0\r\n",
        );
        let (icon, other) = parse_icon_resource(content);
        assert_eq!(icon, Some("myicon.ico,0".to_string()));
        // CustomSection's IconResource preserved
        assert!(
            other.iter().any(|l| l.contains("IconResource=leave_me_alone.ico,0")),
            "IconResource in CustomSection must be preserved"
        );
        // ShellClassInfo icon removed
        assert!(!other.iter().any(|l| l.contains("myicon.ico")));
        // Other sections preserved
        assert!(other.iter().any(|l| l.contains("InfoTip=My Folder")));
        assert!(other.iter().any(|l| l.contains("[ViewState]")));
    }

    // =========================================================================
    // build_content tests
    // =========================================================================

    #[test]
    fn test_build_content_new() {
        let content = build_content_with_icon(None, "D:\\Icons\\test.ico");
        assert!(content.contains("[.ShellClassInfo]"));
        assert!(content.contains("IconResource=D:\\Icons\\test.ico,0"));
    }

    #[test]
    fn test_build_content_replace_existing() {
        let existing = "[.ShellClassInfo]\r\nIconResource=old.ico,0\r\n";
        let content = build_content_with_icon(Some(existing), "D:\\Icons\\new.ico");
        assert!(content.contains("IconResource=D:\\Icons\\new.ico,0"));
        assert!(!content.contains("old.ico"));
    }

    #[test]
    fn test_build_content_preserves_other_section_iconresource() {
        // IconResource in other sections must survive a build_content_with_icon call
        let existing = concat!(
            "[.ShellClassInfo]\r\nIconResource=old.ico,0\r\n",
            "[OtherSection]\r\nIconResource=keep.ico,0\r\n",
        );
        let content = build_content_with_icon(Some(existing), "D:\\Icons\\new.ico");
        assert!(content.contains("IconResource=D:\\Icons\\new.ico,0"), "New icon must be set");
        assert!(content.contains("IconResource=keep.ico,0"), "OtherSection IconResource must survive");
        assert!(!content.contains("old.ico"), "Old ShellClassInfo IconResource must be removed");
    }

    #[test]
    fn test_build_content_without_icon() {
        let existing =
            "[.ShellClassInfo]\r\nIconResource=test.ico,0\r\nInfoTip=Test";
        let content = build_content_without_icon(existing);
        assert!(content.is_some());
        let c = content.unwrap();
        assert!(!c.contains("IconResource"));
        assert!(c.contains("InfoTip=Test"));
    }

    #[test]
    fn test_build_content_without_icon_none() {
        let existing = "[.ShellClassInfo]\r\nInfoTip=Test";
        let content = build_content_without_icon(existing);
        assert_eq!(content, None);
    }

    #[test]
    fn test_build_content_without_icon_preserves_other_section() {
        // Restoring default should only affect ShellClassInfo
        let existing = concat!(
            "[.ShellClassInfo]\r\nIconResource=remove.ico,0\r\n",
            "[OtherSection]\r\nIconResource=keep.ico,0\r\n",
        );
        let content = build_content_without_icon(existing);
        assert!(content.is_some());
        let c = content.unwrap();
        assert!(!c.contains("remove.ico"), "ShellClassInfo IconResource must be removed");
        assert!(c.contains("IconResource=keep.ico,0"), "OtherSection IconResource must survive");
        assert!(c.contains("[.ShellClassInfo]"), "ShellClassInfo header must survive");
    }
}
