<?php
return;
include('cache.php');
Load(30,false);

foreach (glob("music/*") as $filename) {
    echo "$filename\n";
}
//Save();

?>