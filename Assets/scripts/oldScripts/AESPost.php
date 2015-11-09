<?php

$aesData = $_REQUEST["aesData"];
$aesIv = $_REQUEST["aesIv"];
$aesKey = "dkekwmqmckbowoadlelwqeovknkfjsjq";
if(isset($aesIv))
    ob_start();

function addpadding($string)
{
    $blocksize = 32;
    $len = strlen($string);
    $pad = $blocksize - ($len % $blocksize);
    $string .= str_repeat(chr($pad), $pad);
    return $string;
}
function strippadding($string)
{
    $slast = ord(substr($string, -1));
    $slastc = chr($slast);
    $pcheck = substr($string, -$slast);
    if(preg_match("/$slastc{".$slast."}/", $string)){
        $string = substr($string, 0, strlen($string)-$slast);
        return $string;
    } else {
        return false;
    }
}

function encrypt($string)
{
    global $aesIv,$aesKey;
    return base64_encode(mcrypt_encrypt(MCRYPT_RIJNDAEL_256, $aesKey, addpadding($string), MCRYPT_MODE_CBC, $aesIv));
}

function decrypt($aesData)
{
    global $aesIv,$aesKey;
    //echo "$aesData\n$aesIv\n$aesKey\nend\n";
    $aesData = base64_decode($aesData);
    return strippadding(mcrypt_decrypt(MCRYPT_RIJNDAEL_256, $aesKey, $aesData, MCRYPT_MODE_CBC, $aesIv));
}
if(isset($aesData))
{
    $dec = decrypt($aesData);
    $json = json_decode($dec,true);
    if(isset($json) && $json)
    {
        foreach($json as $key => $value) {
            $_REQUEST[$key] = $value;
        }
        //echo $_REQUEST["test"];
    }
    else
        die("Failed read");
}
if(isset($aesIv))
	register_shutdown_function("encOutput");
function encOutput()
{    
        $out2 = ob_get_contents();
        ob_end_clean();
        echo encrypt($out2);   
}



?>