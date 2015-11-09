<?php
include('cache2.php');
Load(3600,true);
include('include.php');

foreach (glob("maps/*") as $filename) {
	echo "$filename:".filemtime($filename)."\n";
}
//Save();

?>