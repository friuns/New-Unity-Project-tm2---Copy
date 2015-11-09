<?php
include('include.php');
$name= get('name');
$password = tryget('password');
$email = tryget('email');
//if(file_exists("players/$name/password.txt"))
//	die("user already exists");

crdir("emails");
if($password)
{
    if(query("select name from tmusers where name ='$name'")->num_rows)
    {
        $perfMsg = "select";
        die("user already exists");
    }
    crdir("players/$name",0777);
    file_put_contents2("players/$name/password.txt",md5($password));
    $md5=md5($password);
    $r = query("insert ignore into tmusers (name,password,email,registerDate) values ('$name','$md5','$email',now())");
    $perfMsg = "insert";
    if (!$mysqli->affected_rows)
        die("user already exists2");
    userfile($name);
	echo("registration success\nid:".mysqli_insert_id($mysqli));
}

if($email)
{
	file_put_contents2("emails.txt","$email\n",FILE_APPEND);
	$email = md5($email);
	file_put_contents2("emails/$email.txt", "$password");
}
//perf();