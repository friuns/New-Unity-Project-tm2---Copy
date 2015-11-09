
<?php

$filename = preg_replace('/[^a-zA-Z0-9_.-]/','', $_SERVER['HTTP_REFERER']);
$date= date('Y-m-d');
crdir("referers/$date/");
file_put_contents2("referers/$date/$filename.txt",'1',FILE_APPEND);

function crdir($name)
{
	if(!file_exists2($name))
		mkdir2($name,0700,true);
}

?>

