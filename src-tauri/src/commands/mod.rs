mod desktop_ini;
mod explorer;

use desktop_ini::{build_content_with_icon, build_content_without_icon, read_desktop_ini, write_desktop_ini};
use explorer::refresh_folder_icon;
use std::fs;

/// Apply an ICO icon to a target folder.
/// Uses desktop.ini with IconResource referencing the source ICO path.
#[tauri::command]
pub fn apply_icon(folder_path: String, ico_path: String) -> Result<String, String> {
    // Validate paths
    let folder = std::path::Path::new(&folder_path);
    if !folder.exists() || !folder.is_dir() {
        return Err(format!("目标文件夹不存在: {}", folder_path));
    }

    let ico = std::path::Path::new(&ico_path);
    if !ico.exists() || !ico.is_file() {
        return Err(format!("ICO 文件不存在: {}", ico_path));
    }

    // Ensure ICO file extension
    if ico.extension().map(|e| e.to_ascii_lowercase()) != Some("ico".into()) {
        return Err("所选文件不是 .ico 格式".to_string());
    }

    // Step 1: Set folder system attribute (required for desktop.ini to take effect)
    set_folder_system_attribute(&folder_path)?;

    // Step 2: Read existing desktop.ini
    let existing_content = read_desktop_ini(&folder_path);

    // Step 3: Build new desktop.ini content
    let new_content = build_content_with_icon(existing_content.as_deref(), &ico_path);

    // Step 4: Write desktop.ini
    write_desktop_ini(&folder_path, &new_content)
        .map_err(|e| format!("写入 desktop.ini 失败: {}", e))?;

    // Step 5: Set desktop.ini file attributes (hidden + system)
    set_ini_file_attributes(&folder_path)?;

    // Step 6: Refresh Explorer to show the new icon
    refresh_folder_icon(&folder_path);

    Ok(format!("图标已应用到: {}", folder_path))
}

/// Restore the default Windows folder icon.
/// Only removes IconResource from desktop.ini, preserves all other settings.
#[tauri::command]
pub fn restore_default_icon(folder_path: String) -> Result<String, String> {
    let folder = std::path::Path::new(&folder_path);
    if !folder.exists() || !folder.is_dir() {
        return Err(format!("目标文件夹不存在: {}", folder_path));
    }

    // Step 1: Read existing desktop.ini
    let existing_content = match read_desktop_ini(&folder_path) {
        Some(content) => content,
        None => {
            return Ok("该文件夹没有 desktop.ini，无需恢复".to_string());
        }
    };

    // Step 2: Remove IconResource
    let new_content = match build_content_without_icon(&existing_content) {
        Some(content) => content,
        None => {
            return Ok("该文件夹未设置自定义图标，无需恢复".to_string());
        }
    };

    // Step 3: Write back (even if empty — never delete desktop.ini)
    write_desktop_ini(&folder_path, &new_content)
        .map_err(|e| format!("写入 desktop.ini 失败: {}", e))?;

    // Step 4: Refresh Explorer
    refresh_folder_icon(&folder_path);

    Ok(format!("已恢复默认图标: {}", folder_path))
}

/// Set the folder's System attribute so Windows processes desktop.ini
fn set_folder_system_attribute(folder_path: &str) -> Result<(), String> {
    use std::os::windows::fs::MetadataExt;
    let metadata = fs::metadata(folder_path)
        .map_err(|e| format!("无法读取文件夹属性: {}", e))?;
    let attrs = metadata.file_attributes();

    const FILE_ATTRIBUTE_SYSTEM: u32 = 4;
    if attrs & FILE_ATTRIBUTE_SYSTEM == 0 {
        // Use Windows API to set the system attribute
        // For now, use attrib command as a fallback
        std::process::Command::new("attrib")
            .args(["+s", folder_path])
            .output()
            .map_err(|e| format!("设置文件夹系统属性失败: {}", e))?;
    }
    Ok(())
}

/// Set desktop.ini file attributes to Hidden + System
fn set_ini_file_attributes(folder_path: &str) -> Result<(), String> {
    let ini_path = std::path::Path::new(folder_path).join("desktop.ini");
    if ini_path.exists() {
        std::process::Command::new("attrib")
            .args(["+s", "+h", ini_path.to_str().unwrap()])
            .output()
            .map_err(|e| format!("设置 desktop.ini 属性失败: {}", e))?;
    }
    Ok(())
}
