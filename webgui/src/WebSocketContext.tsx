// WebSocketContext.tsx
import React, { createContext, useContext } from 'react';
import useWebSocket from 'react-use-websocket';
import { SendJsonMessage } from 'react-use-websocket/dist/lib/types';

// Define the context type
interface WebSocketContextType {
  sendJsonMessage: SendJsonMessage;
  lastJsonMessage: unknown;
  readyState: WebSocket['readyState'];
}

// Create the WebSocket context with an initial undefined value
const WebSocketContext = createContext<WebSocketContextType | undefined>(undefined);

// Define props type for WebSocketProvider, if needed
interface WebSocketProviderProps {
  children: React.ReactNode;
}

export const WebSocketProvider: React.FC<WebSocketProviderProps> = ({ children }) => {
  const { sendJsonMessage, lastJsonMessage, readyState } = useWebSocket('ws://' + location.hostname + ':8081', {
    shouldReconnect: () => true,
  });

  // The value provided to the context consumers
  const value = { sendJsonMessage, lastJsonMessage, readyState };

  return <WebSocketContext.Provider value={value}>{children}</WebSocketContext.Provider>;
};

// Custom hook to use the WebSocket context
// eslint-disable-next-line react-refresh/only-export-components
export const useWebSocketContext = (): WebSocketContextType => {
  const context = useContext(WebSocketContext);
  if (!context) {
    throw new Error('useWebSocketContext must be used within a WebSocketProvider');
  }
  return context;
};
