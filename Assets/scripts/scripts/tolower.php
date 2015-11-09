<?php
return;
$dir = '../players/';
if ($handle = opendir($dir)) {
    echo "Directory handle: $handle\n";
    echo "Entries:\n";

    mkdir("$dir/copy");
    while (false !== ($entry = readdir($handle))) {
        $new = strtolower($entry);
        if($new!=$entry && @!rename($dir.$entry,$dir.$new))
        {
            echo "cannot rename $entry ".(file_exists($dir.$new)?"true":"false");
            if(file_exists($dir.$new))
            {
                echo "remove ".$dir.$entry."\n ";
                if(!rename($dir.$entry,$dir."copy/".$entry))
                    echo "fuck\n";
            }
        }
    }
    closedir($handle);
}