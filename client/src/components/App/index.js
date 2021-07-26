import { useState } from "react";
import LoadClients from "../LoadClients";
import LoadClientDetails from "../LoadClientDetails";
import Configs from "../Configs";
import { ConfigsContext, LoadClientContext } from "../../contexts";

import "./styles.css"

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
    dateCreated: 2019,
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
    dateCreated: 2021,
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
    dateCreated: 2020,
  }
];

const configs = [
  {
    name: "Config 1",
    description: "This is the first config",
    id: 1,
  },
  {
    name: "Config 2",
    description: "This is the second config",
    id: 2,
  },
  {
    name: "Config 3",
    description: "This is the third config",
    id: 3,
  },
  {
    name: "Config 4",
    description: "This is the fourth config",
    id: 4,
  }
]

const loadTests = [
  {
    name: "Load Test 1",
    configId: 1,
    id: 1,
  },
  {
    name: "Load Test 2",
    configId: 2,
    id: 2,
  },
  {
    name: "Load Test 3",
    configId: 1,
    id: 3,
  },
  {
    name: "Load Test 4",
    configId: 3,
    id: 4,
  },
  {
    name: "Load Test 5",
    configId: 4,
    id: 5,
  },
  {
    name: "Load Test 6",
    configId: 2,
    id: 6,
  },
  {
    name: "Load Test 7",
    configId: 1,
    id: 7,
  },
  {
    name: "Load Test 8",
    configId: 3,
    id: 8,
  },
  {
    name: "Load Test 9",
    configId: 1,
    id: 9,
  }
]

function App() {

  const [isOpen, setIsOpen] = useState(false)
  const [ currClientDetails, setCurrClientDetails ] = useState(-1)

  const handleOpen = (index) => {
    setIsOpen(true);
    setCurrClientDetails(index);
  }

  const resetCurrClientDetails = () => {
    setCurrClientDetails(-1)
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
      <ConfigsContext.Provider value={{ configs, loadTests }}>
        <Configs></Configs>
      </ConfigsContext.Provider>
    </div>
  );
}

export default App;
