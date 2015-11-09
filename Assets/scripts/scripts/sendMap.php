<?php
include('include.php');
$name= get('name');
$map = get('map');
$version = get('version');
$thefile = $_FILES['file'];
$flags = intval(tryget('flags',5000,1));
//if($flags)
//    $flags= "flags='$flags'";

if(empty($thefile))  Die("no file");

move_uploaded_file2($thefile['tmp_name'], "usermaps/$name.$map.map") or die('move failed');
echo 'map uploaded';

//$date = date('Y-m-d H:i:s',time());
$r=0;
//if($name && file_exists("promoteMaps.txt") && in_array(strtolower($name),explode("\n",file_get_contents("promoteMaps.txt"))))
//    $r=5;

$mysqli->query("insert into tmusermaps (name,date,user,map,version,rate,cnt,flags) values ('$name.$map',Now(),'$name','$map','$version','$r','$r','$flags') on duplicate key update date=Now(), version='$version',tested='0', cnt='$r', rate='$r' ,flags='$flags'") or trigger_error(mysqli_error($mysqli));


//$mysqli->query("delete from tmreplays where mapN = '$name.$map'");
//while($row = $result->fetch_object()) {
//	unlink($row->name);
//	echo "delete ".$row->name;
//}
//perf();