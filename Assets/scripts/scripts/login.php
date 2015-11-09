<?php

include('include.php');

$name= get('name');
$password = get('password');
$md5 = md5($password);

if(!file_exists2("players/$name/password.txt"))
	die("user not exists");

$pass =file_get_contents2("players/$name/password.txt");
if($md5 != $pass && $password != $pass)
	echo("wrong password\r\n");
else
{
	echo("login success\r\n");
}