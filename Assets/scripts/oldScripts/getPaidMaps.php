<?php
header("Content-Type: text/plain");
foreach (glob("{maps/paidMaps/*.unity3d*}",GLOB_BRACE) as $filename) 
{
	$date = date('Y-m-d',filectime($filename));
	echo "$filename\r\n";
}
?>