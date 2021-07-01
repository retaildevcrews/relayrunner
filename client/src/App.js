import { useState } from "react";
import LoadClients from "./LoadClients"
import LoadClientDetails from "./LoadClientDetails";

function App() {

  const loadclients = [
    {
      name: "Load-Client-ID-001",
      version: 5.0,
      id: 1,
      region: "california",
      zone: "colorado",
      scheduler: "nev",
      currstatus: "ready",
      metrics: "prometheus",
      isExecute: false,
    },
    {
      name: "Load-Client-ID-002",
      version: 5.0,
      id: 2,
      region: "california",
      zone: "colorado",
      scheduler: "nev",
      currstatus: "unresponsive",
      metrics: "prometheus",
      isExecute: false,
    },
    {
      name: "Load-Client-ID-003",
      version: 5.0,
      id: 3,
      region: "california",
      zone: "colorado",
      scheduler: "nev",
      currstatus: "busy",
      metrics: "prometheus",
      isExecute: false,
    }
  ];

  const [isOpen, setIsOpen] = useState(false)
  const [ currClientDetails, setCurrClientDetails ] = useState(-1)

  const resetCurrClientDetails = () => {
    setCurrClientDetails(-1)
  }
 
  const handleOpen = (index) => {
    setIsOpen(true);
    setCurrClientDetails(index);
  }

  const handleClose = () => {
    setIsOpen(false);
    resetCurrClientDetails();
  }

  return (
    <div className="App">
      <LoadClients openPopup={handleOpen} clientDetails={loadclients}/> 
      {isOpen && <LoadClientDetails
          clientDetails={loadclients}
          clientIndex={currClientDetails}
          closePopup={handleClose}
        />}
    </div>
  );
}

export default App;

