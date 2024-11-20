import 'react-tabs/style/react-tabs.css';
import './App.css'
import { DeviceList } from './Pages/DeviceList';
import { JoinNetworkSection } from './Pages/JoinNetworkSection';

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
