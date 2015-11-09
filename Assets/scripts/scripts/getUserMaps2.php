<?php

include('cache2.php');
Load(3600);

include('include.php');


$page = min(tryget('page',99,0),3)*30;
$user = tryget('user');
$direct = tryget('direct');
$version = tryget('version',5000,0);
$map = tryget('map');
$tested = tryget('tested',5000,0);
$eq =  tryget('exact')? "version = $version":"version <= $version";
$top = tryget('top');
/*$flags = intval(tryget('flags',5000,""));
$flags2 ="";
if($flags)
    $flags2 = "and flags>=$flags && flags&$flags";
else if(!$map && !$user)
    $flags2 = "and flags<=8";*/
$q='';

if($map)
{
    if($direct)
    {
        $a = explode('.',$map);
        $q= "select * from tmusermaps where user='$a[0]' and map='$a[1]' order by date desc";
    }
    else if(strlen($map)<3)
        die("too small");
    else
        $q = "select * from tmusermaps where map like '$map%' limit 15 union all select * from tmusermaps where user like '$map%' order by date desc limit 30";
}
else if($user)
    $q = "select * from tmusermaps where user = '$user' order by date desc limit 30 ";
else
{
    $order = $top?'and advanced = 1 order by rate2 desc':'order by date desc';
    //if($tested ==2 && rand(0,1)>.5)
    //    $order = "RAND(day(now()))";
    $q = "select * from tmusermaps where tested=$tested $order limit $page,30";
}

//$rate = $advanced?"advanced = 1 and":"";//cnt>4 and NOW() - INTERVAL 10 day<date

$result= query($q) or die("error");

while($row = $result->fetch_object()) {
	echo 'usermaps/'.$row->name.'.map;'. ($row->cnt>2 || $top ? $row->rate:0).';'.$row->flags.';'.$row->id. "\r\n";
}

?>  