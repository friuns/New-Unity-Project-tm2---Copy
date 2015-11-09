<?php

include('include.php');



$name= get('name');
$password = get('password');
$md5 = md5($password);
//$device = get('deviceId');

$r = query("select * from tmusers where name='$name'") or trigger_error("");
$f = $r->fetch_assoc();
if($f && ($f['password'] == $password || $f['password'] == $md5|| $admin))
{
    echo "login success2\n";
    //$mysqli->query("insert IGNORE into tmUserDevice (name,device) values ('$name','$_SERVER[REMOTE_ADDR]')");
    foreach ($f as $key => $value)
        echo "$key:$value\n";
    perf("query");
    userfile($name,true);
}
else if(!$f)
    echo "user not exist";
else
    echo "wrong password";



perf('end');





////obsolete
//if(!file_exists("players/$name/password.txt"))
//    die("user not exists");
//
//$pass =file_get_contents("players/$name/password.txt");
//if($md5 != $pass && $password != $pass)
//    echo("wrong password\n");
//else
//{
//    echo("login success\n0\n0\n0\n0\n0\n0\n0\n0\n");
//    $mysqli->query("insert into tmusers (name,password,registerDate) values ('$name','$md5',now())");
//}
