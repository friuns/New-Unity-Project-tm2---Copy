<?php
date_default_timezone_set('UTC');
echo date("Y-m-d H:i:s");




function xor_this($string) {
    $key = 'fptasdjklfjekljfkajsdsa';
    $text =$string;
    $outText = '';
    for($i=0;$i<strlen($text);)
        for($j=0;$j<strlen($key);$j++,$i++)
            $outText .= ($text[$i] ^ $key[$j]);
    return $outText;
}
?>