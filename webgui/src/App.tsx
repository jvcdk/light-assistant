import './App.css'
import { Device } from './Device';
import MyComponent from './MyComponent';

function MyButton() {
  return (
    <button>I'm a button</button>
  );
}

function App() {
  return (
    <div>
      <h1>Light Assistant</h1>
      <MyButton />
      <Device name="My Device" />
      <MyComponent />
    </div>
  )
}

export default App
