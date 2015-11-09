<?php


include('include.php');
foreach(glob("replays/*.rep") as $f)
{
	$a = explode('/',$f);
	$e = explode('.',$a[1]);
	$date = date('Y-m-d H:i:s',filectime($f));
	if($mysqli->query("insert into tmreplays (name,date,user,map) values ('$f','$date','$e[0]','$e[1]')"))
		echo "added $f\r\n";	
	file_put_contents2("players2/$e[0].txt","");
	
}
echo mysqli_error($mysqli);


?>