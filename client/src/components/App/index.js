import { useState } from "react";
import LoadClients from "../LoadClients";
import LoadClientDetails from "../LoadClientDetails";
import LoadClientContext from "../LoadClientContext";

const loadClients = [
  {
    name: "Load-Client-ID-001",
    version: 5.0,
    id: 1,
    region: "california",
    zone: "colorado",
    scheduler: "nev",
    currstatus: "ready",
    metrics: "prometheus",
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
  }
];

function App() {

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
      <LoadClientContext.Provider 
        value={{ loadClients }}>
        <LoadClients handleOpen={handleOpen}/> 
        {isOpen && <LoadClientDetails handleClose={handleClose} currClientDetails={currClientDetails}/>}
      </LoadClientContext.Provider>
    </div>
  );
}

export default App;

