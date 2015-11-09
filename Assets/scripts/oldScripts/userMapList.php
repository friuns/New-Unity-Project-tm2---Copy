<?php
header("Content-Type: text/plain");
foreach (glob("*.dae*") as $filename) 
{
	$date = date('Y-m-d',filectime($filename));
	echo "$filename#$date\r\n";
}
?>