<?php
include('include.php');
$map = get("map");
$send = $_REQUEST["send"];
crdir("chat");
$f = "chat/".$map.".txt";

$data = file_exists2($f)? file_get_contents2($f):"";
$len = strlen($data);
if($len>1000)
	$data = substr($data, $len-1000);
file_put_contents2($f,"$data\n$send");
