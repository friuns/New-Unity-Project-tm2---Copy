<?php

include('include.php');



$name= get('name');
$password = get('password');
$md5 = md5($password);

$r = $mysqli->query("select reputation,score,friends from tmusers where name='$name' and password='$md5'") or trigger_error("");
$f = $r->fetch_object();
if($f)
    die("login success2\n$f->reputation\n$f->score\n$f->friends");











//obsolete
if(!file_exists2("players/$name/password.txt"))
    die("user not exists");

$pass =file_get_contents2("players/$name/password.txt");
if($md5 != $pass && $password != $pass)
    echo("wrong password\n");
else
{
    echo("login success\n0\n0\n0\n0\n0\n0\n0\n0\n");
    $mysqli->query("insert into tmusers (name,password) values ('$name','$md5')");
}
