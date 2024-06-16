import './App.css'
import { DeviceList } from './DeviceList';
import { JoinNetworkSection } from './JoinNetworkSection';

function App() {
  return (
    <div>
      <h1>Light Assistant</h1>
      <DeviceList />
      <JoinNetworkSection />
    </div>
  )
}

export default App
