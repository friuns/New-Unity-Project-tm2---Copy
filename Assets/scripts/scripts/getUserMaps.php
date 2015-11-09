<?php
return;

include('include.php');



$page = tryget('page',5000,0)*30;

$map = '';//tryget('map');
$version = tryget('version',5000,0);
$tested = tryget('tested',5000,0);

$eq =  tryget('exact')? '=':'<=';

if($map)
	$map= "name like '%$map%' and ";



$result= query("select name from tmusermaps where $map version $eq $version and tested>=$tested order by date desc limit $page,30") or die("error");


while($row = $result->fetch_object()) {
	echo $row->name. "\r\n";
}

?> 