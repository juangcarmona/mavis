<?php
/**
 * Shortcode: [mavis_cam camera="test_cam_1" interval="5"]
 * Loads a fresh (cache-busted) frame before showing anything; then auto-refreshes.
 */
if ( ! function_exists( 'mavis_cam_shortcode' ) ) {
	function mavis_cam_shortcode( $atts ) {
		$atts = shortcode_atts(
			array(
				'camera'   => 'default',
				'interval' => 5, // seconds
			),
			$atts,
			'mavis_cam'
		);

		$camera   = sanitize_title( $atts['camera'] );
		$interval = max(1, intval($atts['interval']));
		$id       = 'mavis_cam_' . $camera;
		$base_url = esc_url( home_url( "/wp-content/uploads/mavis/{$camera}/latest.jpg" ) );

		// Transparent 1x1 GIF placeholder (prevents layout shift; nothing cached is shown).
		$placeholder = 'data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///ywAAAAAAQABAAACAUwAOw==';

		ob_start(); ?>
		<div class="mavis-cam-wrapper" style="text-align:center;">
			<img id="<?php echo esc_attr($id); ?>"
			     src="<?php echo $placeholder; ?>"
			     alt="Camera <?php echo esc_attr($camera); ?>"
			     style="max-width:100%;height:auto;border:1px solid #444;opacity:0;transition:opacity .15s ease"/>
		</div>
		<script>
		(function(){
		  const img = document.getElementById('<?php echo esc_js($id); ?>');
		  const base = '<?php echo $base_url; ?>';

		  function freshUrl() {
		    return base + '?cb=' + Date.now() + Math.random().toString(36).slice(2);
		  }

		  // Preload next frame off-DOM, then swap in and reveal (no cached flash)
		  function swapFresh() {
		    const tmp = new Image();
		    const url = freshUrl();
		    tmp.onload = function(){
		      img.src = url;
		      if (img.style.opacity !== '1') img.style.opacity = '1';
		    };
		    tmp.decoding = 'async';
		    tmp.src = url;
		  }

		  // Initial load: fetch fresh frame first, keep placeholder hidden until ready
		  swapFresh();

		  // Periodic refresh
		  setInterval(swapFresh, <?php echo $interval * 1000; ?>);
		})();
		</script>
		<?php
		return ob_get_clean();
	}
	add_shortcode( 'mavis_cam', 'mavis_cam_shortcode' );
}
