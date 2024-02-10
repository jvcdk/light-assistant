import React from 'react';
import { useWebSocketContext } from './WebSocketContext';

const MyComponent: React.FC = () => {
  const { sendMessage, lastMessage } = useWebSocketContext();

  const handleSendMessage = () => {
    sendMessage('Hello WebSocket!');
  };

  return (
    <div>
      <button onClick={handleSendMessage}>Send Message</button>
      {lastMessage && <p>Last JVC Message: {lastMessage.data}</p>}
    </div>
  );
};

export default MyComponent;
