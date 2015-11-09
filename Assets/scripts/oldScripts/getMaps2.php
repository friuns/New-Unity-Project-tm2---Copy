<?php
header("Content-Type: text/plain");
foreach (glob("{*.unity3d*,ported/*.unity3d*}",GLOB_BRACE) as $filename) 
{
	$date = date('Y-m-d',filectime($filename));
    echo "$filename#$date\r\n";

}
?>