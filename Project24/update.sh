
echo "Next Path: $1"
echo "App Path: $2"
echo 

echo "Sending command to stop service.."
sudo systemctl stop kestrel-p24app.service

while :
do
    status=`systemctl show -p SubState kestrel-p24app.service --value`
    if [[ $status != "dead" ]]
    then
        echo "    Waiting for service to stop.."
        sleep 1
    else
        break
    fi
done
echo $'Service stopped.\n'

echo "Copying files.."

rsync -r -p "$1" "$2"
errCode=$?
if [[ $errCode == 0 ]]
then
    echo $'Files copied ('$errCode$').\n'
else
    echo $'Rsync error ('$errCode$').\n'
fi

echo "Restarting service.."
sudo systemctl start kestrel-p24app.service

while :
do
    status=`systemctl show -p SubState kestrel-p24app.service --value`

    if [[ $status == "dead" ]]
    then
        echo "    Service is still dead.."
        sleep 1
    else
        break
    fi
done
echo "Service restarted. Updating done!"
