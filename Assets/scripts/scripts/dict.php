<?php
return;
include('include.php');

$key = get('key');
$value= get('value');

crdir("dict/");
file_put_contents2("dict/$key.txt",$value);