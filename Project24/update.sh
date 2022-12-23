#!/bin/bash

sleep 1

echo "$(date +'%Y/%m/%d %H:%M:%S'): Starting script.."
echo "    switch   : $1"
echo "    PID      : $2"
echo "    App Path : $3"
echo "    Next Path: $4"

if [[ $1 == "-launchUpdater" ]]
then
    # start updater (launcher)
    echo "$(date +'%Y/%m/%d %H:%M:%S'): Sending command to start updater service.."
    sudo systemctl start p24app-updater.service
elif [[ $1 == "-quick" ]]
then
    # do quick update ()
    echo "$(date +'%Y/%m/%d %H:%M:%S'): Quick update.."

    rsync -rpvv "$4" "$3"
    errCode=$?
    if [[ $errCode == 0 ]]
    then
        echo $'    Files copied ('$errCode$').\n'
    else
        echo $'    Rsync error ('$errCode$').\n'
    fi
    
    echo "$(date +'%Y/%m/%d %H:%M:%S'): Updating done!"
elif [[ $1 == "-main" ]]
then
    # do update here..
    echo "$(date +'%Y/%m/%d %H:%M:%S'): Starting update.."

    # From rsync's man page: 
    #   A trailing / on a source name means "copy the contents of this directory".
    #   Without a trailing slash it means "copy the directory".

    rsync -rpvv "$4" "$3"
    errCode=$?
    if [[ $errCode == 0 ]]
    then
        echo $'    Files copied ('$errCode$').\n'
    else
        echo $'    Rsync error ('$errCode$').\n'
    fi
    
    echo "$(date +'%Y/%m/%d %H:%M:%S'): Files updating success."

    # start main app
    echo "$(date +'%Y/%m/%d %H:%M:%S'): Sending command to start Project24 app.."
    sudo systemctl start kestrel-p24app.service

    echo "$(date +'%Y/%m/%d %H:%M:%S'): Updating done!"
else
    echo "$(date +'%Y/%m/%d %H:%M:%S'): Invalid first argument."
fi
