<?php

include('include.php');
$name= get('name');
$vkpassword = get('vkPassword');
$oldPassword = get('password');
if(!$oldPassword && !$vkpassword) die("<error uid is empty>");
$md5old = md5($oldPassword);


$r = query("select reputation,score,friends,vkpassword,password from tmusers where name='$name' ") or trigger_error("");
$f = $r->fetch_object();

if($f)
{
    if($f->password === $md5old || $f->vkpassword === $vkpassword)
    {
        if(!$f->vkpassword)
            query("update tmusers set vkpassword='$vkpassword' where name= '$name'");
        die("login success2\n$f->reputation\n$f->score\n$f->friends");
    }
    else
        die("user already registered");
}
else
{
    query("insert into tmusers (name,vkpassword) values ('$name','$vkpassword')");
    die("registered");
}



//
//if(file_exists("players/$name"))
//{
//	$pass =file_get_contents("players/$name/password.txt");
//	if($md5vk != $pass && $password != $pass)
//		die("пользователь с таким именем уже существует");
//}
//else
//{
//	crdir("players/$name");
//	file_put_contents("players/$name/password.txt",md5($password));
//}
//die("login success");
