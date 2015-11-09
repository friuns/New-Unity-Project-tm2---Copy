<?php

include('include.php');
include('includeAcc.php');
$name= get('name');
//$password = get('password');
//if(!$password) die("no password");
$f = "players/$name/prefs.txt";

$thefile = $_FILES['file'];
if(!empty($thefile))
{
    crdir("players/$name/");
    CheckAcc();
    perf("Check Acc");
    if(!file_exists2($f) || $_FILES['file']['size']>filesize2($f)/2)
    {
        move_uploaded_file2($_FILES['file']['tmp_name'], $f);
        perf("move Uploaded file");
        echo 'prefs uploaded';
    }
    else
        echo 'uploaded file is too small';
}
else
{
    echo 'Failed!';
}
//perf();