<?php
include('include.php');
$map = get('map');
$tested=1;
if(tryget('remove'))
	$tested=0;
query("update tmusermaps set tested=$tested where name='$map'") or trigger_error();