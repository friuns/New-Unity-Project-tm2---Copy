<?php
header("Content-Type: text/plain");
$ext = @$_GET["ext"];
if(!$ext) $ext = "{*.zip,*.unity3d,*.apk}";

//foreach(glob("builds/*/$ext",GLOB_BRACE) as $file) {
//	$ext = pathinfo($file, PATHINFO_EXTENSION);
//	$file = "http://206.190.128.180/server/$file";	
//	if($ext == "unity3d")	
//		echo "http://206.190.128.180/server/play.html?url=$file\r\n";
//	else
//		echo "$file\r\n";
	
//}