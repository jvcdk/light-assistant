// WebSocketContext.tsx
import React, { createContext, useContext } from 'react';
import useWebSocket from 'react-use-websocket';

// Define the context type
interface WebSocketContextType {
  sendMessage: (message: string | ArrayBufferLike | Blob | ArrayBufferView) => void;
  lastMessage: MessageEvent | null;
  readyState: WebSocket['readyState'];
}

// Create the WebSocket context with an initial undefined value
const WebSocketContext = createContext<WebSocketContextType | undefined>(undefined);

// Define props type for WebSocketProvider, if needed
interface WebSocketProviderProps {
  children: React.ReactNode;
}

export const WebSocketProvider: React.FC<WebSocketProviderProps> = ({ children }) => {
  const { sendMessage, lastMessage, readyState } = useWebSocket('wss://echo.websocket.org/', {
    shouldReconnect: () => true,
  });

  // The value provided to the context consumers
  const value = { sendMessage, lastMessage, readyState };

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
