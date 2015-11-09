<?php
//die("sorry account server is offline, You can play as guest. This will be fixed soon");
include('include.php');
$name= get('name');
$vkpassword = get('vkPassword');
$oldPassword = get('password');
if(!$oldPassword && !$vkpassword) die("<error uid is empty>");
$md5old = md5($oldPassword);


$r = query("select * from tmusers where name='$name' ") or trigger_error("");
$f = $r->fetch_assoc();

if($f)
{

    if(($f['password'] === $oldPassword || $f['password'] === $md5old)|| $f['vkpassword'] === $vkpassword|| $admin)
    {
        if(!$f['vkpassword'] && !$admin)
            query("update tmusers set vkpassword='$vkpassword' where name= '$name'");
        $perfMsg ="select";
        echo "login success2\n";
        //$mysqli->query("insert IGNORE into tmUserDevice (name,device) values ('$name','$_SERVER[REMOTE_ADDR]')");
        foreach ($f as $key => $value)
            echo "$key:$value\n";
        userfile($name,true);
        die();
    }
    else
        die("user already registered".($f['vkpassword']?" vk":""));
}
else
{
    $perfMsg ="insert";
    query("insert into tmusers (name,vkpassword,registerDate) values ('$name','$vkpassword',now())");
    userfile($name);
    die("registered\nid:".mysqli_insert_id($mysqli));
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
