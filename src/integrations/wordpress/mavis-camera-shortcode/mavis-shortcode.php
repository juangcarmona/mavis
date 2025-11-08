<?php
/**
 * Plugin Name: MAVIS Camera Shortcode
 * Plugin URI: https://jgcarmona.com/
 * Description: Displays a live-updating camera image from MAVIS uploads. 
 *              Expected path: /wp-content/uploads/mavis/<camera_name>/latest.jpg 
 *              (<camera_name> must be lowercase).
 * Version: 1.0
 * Author: Juan G Carmona
 * License: GPLv2 or later
 */

if ( ! defined( 'ABSPATH' ) ) exit; // Prevent direct access

require_once plugin_dir_path( __FILE__ ) . 'mavis_cam_shortcode.php';
