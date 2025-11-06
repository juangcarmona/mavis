# MAVIS WordPress Integration

This integration lets you display live, non-cached camera images uploaded by MAVIS directly in your WordPress site, bypassing all CDN and browser caching.

---

## Overview

MAVIS uploads fresh camera snapshots (e.g. `latest.jpg`) to your WordPress `/wp-content/uploads/mavis/<camera_name>/` folder.
This shortcode loads those images dynamically and refreshes them every few seconds, **always fetching the newest frame**.

---

## Installation

You have three options to install the shortcode in your WordPress site.
I recoommeend **Option 3** for best modularity and update safety. (This is what I use in all my sites)

### Option 1 — Quick (direct theme edit)

1. In WordPress Admin, go to **Appearance → Theme File Editor**.
2. Select your active theme (e.g. *GeneratePress*).
3. Open the `functions.php` file.
4. Scroll to the bottom and paste the shortcode code from [`mavis_cam_shortcode.php`](./mavis_cam_shortcode.php).
    NOTE: remove the initial '<?php'  
5. Click **Update File**.
6. Add the shortcode to any page or post:

   ```
   [mavis_cam camera="test_cam_1" interval="5"]
   ```

✅ Works instantly.
⚠️ Will be lost if the theme is updated — use a child theme for permanence.

---

### Option 2 — Safer (child theme)

If you’re using a **child theme** (recommended):

1. Upload `mavis_cam_shortcode.php` file to your child theme folder, e.g.:

   ```
   /wp-content/themes/generatepress-child/mavis_cam_shortcode.php
   ```
2. Add this line at the bottom of your child theme’s `functions.php`:

   ```php
   require_once get_stylesheet_directory() . '/mavis_cam_shortcode.php';
   ```
3. Add the shortcode to a page:

   ```
   [mavis_cam camera="test_cam_1" interval="5"]
   ```

✅ Survives parent theme updates.

---

### Option 3 — Plugin version

1. In your SFTP or local WordPress install, create a folder:

   ```
   /wp-content/plugins/mavis-shortcode/
   ```
2. Inside it, place both:

   * `mavis-shortcode.php`
   * `mavis_cam_shortcode.php`
3. In `mavis-shortcode.php`, add:

   ```php
   <?php
   /**
    * Plugin Name: MAVIS Camera Shortcode
    * Description: Adds [mavis_cam] shortcode for live webcam integration.
    * Version: 1.0
    */

   require_once plugin_dir_path(__FILE__) . 'mavis_cam_shortcode.php';
   ```
4. Go to **Plugins → Installed Plugins**, and activate *MAVIS Camera Shortcode*.

✅ Modular and update-proof.

---

## Usage

Embed the shortcode anywhere:

```markdown
[mavis_cam camera="test_cam_1" interval="5"]
```

**Parameters:**

| Parameter  | Description                          | Default   |
| ---------- | ------------------------------------ | --------- |
| `camera`   | Folder name inside `/uploads/mavis/` | `default` |
| `interval` | Seconds between refreshes            | `5`       |

Example output:

```html
<img src="https://yourdomain.com/wp-content/uploads/mavis/test_cam_1/latest.jpg?cb=1699980123001" />
```

---

## How It Works

* First load shows a transparent placeholder.
* The shortcode preloads the newest image (`latest.jpg?cb=<timestamp>`) before showing it.
* Every *N* seconds (defined by `interval`), a fresh image is preloaded and swapped in without flicker.

This ensures no cached or CDN-stale frame is ever displayed — even on WordPress.com environments using `i0.wp.com` CDN rewriting.

---

## Recommended Folder Structure

When using MAVIS SFTP uploads:

```
/wp-content/uploads/mavis/
   ├── test_cam_1/
   │   ├── latest.jpg
   │   ├── 20251106123000.jpg
   │   └── 20251106123500.jpg
   └── test_cam_2/
       ├── latest.jpg
       └── 20251106124000.jpg
```

---

## Troubleshooting

* **Image doesn’t refresh:** CDN cache too aggressive — ensure MAVIS uploads overwrite `latest.jpg` directly.
* **Old frame flashes for a second:** use the latest version of `mavis_cam_shortcode.php` which preloads before showing.
* **403 or 429 errors:** confirm SFTP upload path is `/htdocs/wp-content/uploads/mavis/...` and credentials are valid.

---

## Credits

Developed as part of the **MAVIS Runtime** toolchain by [J.G. Carmona](https://jgcarmona.com).
