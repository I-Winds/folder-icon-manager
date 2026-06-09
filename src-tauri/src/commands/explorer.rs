/// Refresh Explorer to show icon changes.
/// Uses SHChangeNotify to notify the shell of the change.
pub fn refresh_folder_icon(folder_path: &str) {
    // Strategy: Use multiple notification methods for maximum compatibility.
    // Method 1: SHChangeNotify with SHCNE_UPDATEDIR for the specific folder
    // Method 2: SHChangeNotify with SHCNE_ASSOCCHANGED for global icon refresh

    unsafe {
        use windows_sys::Win32::UI::Shell::{
            SHChangeNotify, SHCNE_ASSOCCHANGED, SHCNE_UPDATEDIR, SHCNF_PATHW, SHCNF_FLUSH,
        };

        // Method 1: Notify specific folder update
        let wide_path: Vec<u16> = folder_path
            .encode_utf16()
            .chain(std::iter::once(0))
            .collect();

        SHChangeNotify(
            SHCNE_UPDATEDIR as i32,
            (SHCNF_PATHW | SHCNF_FLUSH) as u32,
            wide_path.as_ptr() as *const _,
            std::ptr::null(),
        );

        // Method 2: Broadcast icon association change for safety
        SHChangeNotify(
            SHCNE_ASSOCCHANGED as i32,
            SHCNF_FLUSH as u32,
            std::ptr::null(),
            std::ptr::null(),
        );
    }
}
