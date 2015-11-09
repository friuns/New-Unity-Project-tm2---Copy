<?php
include('cache2.php');Load(3600,true);
include('include.php');
function scandir_through($dir)
{
    $items = glob($dir . '*');

    for ($i = 0; $i < count($items); $i++) {
        if (is_dir($items[$i])) {
            $add = glob($items[$i] . '/*');
            if($add)
                $items = array_merge($items, $add);
        }
    }

    return $items;
}

foreach (scandir_through('docs/textures/') as $filename) {
    if(endsWith($filename,'.jpg')|| endsWith($filename,'.png'))
        echo "$filename\n";
}



function endsWith($haystack, $needle)
{
    return $needle === "" || strtolower(substr($haystack, -strlen($needle))) === strtolower($needle);
}