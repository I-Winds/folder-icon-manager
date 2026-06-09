use tauri;

mod commands;

pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_dialog::init())
        .invoke_handler(tauri::generate_handler![
            commands::apply_icon,
            commands::restore_default_icon,
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
