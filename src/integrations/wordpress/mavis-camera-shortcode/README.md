# MAVIS WordPress Integration

Display live, non-cached camera images uploaded by [MAVIS](https://github.com/juangcarmona/mavis) (or any compatible uploader) directly in WordPress — bypassing CDN and browser caching entirely.

---

## Overview

[MAVIS](https://github.com/juangcarmona/mavis) periodically uploads camera snapshots (`latest.jpg`) into
`/wp-content/uploads/mavis/<camera_name>/`.
This plugin provides the `[mavis_cam]` shortcode, which dynamically refreshes those images every few seconds, **always fetching the newest frame**.

---

## Installation

### Option — Standard Plugin Install (recommended)

1. Download **`mavis-camera-shortcode.zip`**
   (the packaged plugin folder containing `mavis-shortcode.php`, `mavis_cam_shortcode.php`, and `README.md`).

2. In WordPress Admin, go to
   **Plugins → Add New → Upload Plugin**

3. Select `mavis-camera-shortcode.zip`, click **Install Now**, then **Activate**.

✅ Works immediately
✅ Survives theme and WordPress updates
✅ No file editing required

---

## Usage

Place the shortcode in any page, post, or block:

```
[mavis_cam camera="test_cam_1" interval="5"]
```

**Parameters**

| Parameter  | Description                                                | Default   |
| ---------- | ---------------------------------------------------------- | --------- |
| `camera`   | Folder name inside `/uploads/mavis/` *(must be lowercase)* | `default` |
| `interval` | Seconds between refreshes                                  | `5`       |

Example rendered output:

```html
<img src="https://yourdomain.com/wp-content/uploads/mavis/test_cam_1/latest.jpg?cb=1699980123001" />
```

---

## How It Works

* Starts with a transparent placeholder to avoid flicker.
* Preloads `latest.jpg?cb=<timestamp>` before showing it.
* Automatically refreshes every *N* seconds.
* Ensures no cached or CDN-stale frame is ever displayed, even under WordPress CDN rewriting (`i0.wp.com`, etc.).

---

## Recommended Folder Structure

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

* **Image doesn’t refresh** → Ensure MAVIS overwrites `latest.jpg` directly.
* **Old frame flashes** → Use the latest plugin build (preloads before swap).
* **403 / 429 errors** → Check upload path `/htdocs/wp-content/uploads/mavis/...` and credentials.

---

## Credits

Developed as part of the **MAVIS Runtime** toolchain by [Juan G. Carmona](https://jgcarmona.com).
