import { useEffect, useState } from 'react';
import './JoinNetworkSection.css'
import { useWebSocketContext } from './WebSocketContext';
import { ClientToServerMessage, IOpenNetworkStatus, IServerToClientMessage } from './JsonTypes';

export function JoinNetworkSection() {
    const { sendJsonMessage, lastJsonMessage } = useWebSocketContext();
    const [networkOpenSeconds, setNetworkOpenSeconds] = useState<number>(0);

    function HandleMessageFromServer(data: IOpenNetworkStatus) {
        const isOpen = data.Status && data.Time > 0;
        if(isOpen)
            setNetworkOpenSeconds(data.Time);
        else
            setNetworkOpenSeconds(0);
    }

    useEffect(() => {
        if(networkOpenSeconds == 0)
            return;

        setTimeout(() => {
            setNetworkOpenSeconds(networkOpenSeconds - 1);
        }, 1000);
    }, [networkOpenSeconds]);

    useEffect(() => {
        if (lastJsonMessage == undefined)
            return
      
        try {
        const message = lastJsonMessage as IServerToClientMessage;
        if(message.OpenNetworkStatus)
            HandleMessageFromServer(message.OpenNetworkStatus);
        } catch (error) {
            console.log(`Error message: ${error}`)
        }
    }, [lastJsonMessage]);

    function SendJoinRequest(): void {
        const msg = new ClientToServerMessage();
        msg.RequestOpenNetwork = true;
        sendJsonMessage(msg);
    }
    
    const isOpen = networkOpenSeconds > 0;
    const status = isOpen ? `Open (${networkOpenSeconds})` : "Closed"
    return (
        <div className='JoinNetwork'>
            <div>
                <label className='Label'>Network status:</label>
                <span className='Value'>{status}</span>
            </div>
            <input disabled={isOpen} type='button' value="Open network" onClick={SendJoinRequest} />
        </div>
    )
}
