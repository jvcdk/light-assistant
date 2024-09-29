import 'react-tabs/style/react-tabs.css';
import './App.css'
import { DeviceList } from './DeviceList';
import { JoinNetworkSection } from './JoinNetworkSection';
import { Tab, Tabs, TabList, TabPanel } from 'react-tabs';
import { Schedule } from './Schedule';

function App() {
  return (
    <div>
      <h1>Light Assistant</h1>
      <Tabs forceRenderTabPanel className='MainContent'>
        <TabList>
          <Tab>Devices</Tab>
          <Tab>Schedule</Tab>
          <Tab>Network</Tab>
        </TabList>
        <TabPanel><DeviceList /></TabPanel>
        <TabPanel><Schedule /></TabPanel>
        <TabPanel><JoinNetworkSection /></TabPanel>
      </Tabs>
    </div>
  )
}

export default App
