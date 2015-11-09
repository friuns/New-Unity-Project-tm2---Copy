<?php
$timeStart=microtime(true);
$fcache='';
$memcache = new Memcache; //point 2.

$memcache->connect("localhost", 11211);
$cacheTime=60;
function Load($time = 60,$Query=true)
{
    global $fcache, $cacheTime,$memcache;
    $cacheTime=$time;
    $fcache =strrchr($_SERVER['PHP_SELF'],'/');

    if($Query)
        $fcache =$fcache.(is_string($Query)?$Query:http_build_query($_REQUEST));
    //$fcache = "cache/$fcache.txt";


    $c = $memcache->get($fcache);
    //file_put_contents("../cached.txt",($c?"load":"save")."$fcache\n",FILE_APPEND);
    if($c)
    {
        if(isset($_REQUEST['a']))
        {
            global $timeStart;
            echo "\ncache:".(microtime(true)-$timeStart)*1000;
        }
        die($c);
    }

    ob_start();
    register_shutdown_function('Save');
}
function Save()
{
    global $fcache,$memcache,$cacheTime;
    $memcache->set($fcache,  ob_get_contents(), false, $cacheTime); //point 3
}


