import { useState } from 'react';


import "./styles.css"

const LoadClients = (props) => {

  const [clients, setClients] = useState(props.clientDetails);

  function handleToggleSelected(id) {
    const newClients = clients.map((lc) => {
      if (lc.id === id) {
        const updatedClient = {
          ...lc,
          isExecute: !lc.isExecute,
        };

        return updatedClient;
      }

      return lc;
    });

    setClients(newClients);
  }

    return (
        <>
        <div className="sidenav">
          <div className="header">
          <div>
              <h1>Load Clients</h1>
          </div>
          <div>
            <select>
              <option value="0">Sort By:</option>
              <option value="1">Name</option>
              <option value="2">Last Updated</option>
              <option value="3">Date Created</option>
            </select>
          </div>
          <div id="filter">
            <select>
              <option value="0">Filter By:</option>
              <option value="1">Ready</option>
              <option value="2">Unresponsive</option>
              <option value="3">Busy</option>
            </select>
          </div>
          </div>
          <hr></hr>
          <div>
            <ul>
              {
                clients.map((lc, index) => (
                    <li key={lc.id}>
                      <button className={`loadclient ${lc.isExecute ? "selected" : ""}`} onClick={() => handleToggleSelected(lc.id)}>{lc.name}</button>
                      <div className="divider"></div> 
                      <button className={`load-client-status ${lc.currstatus}`} title={lc.currstatus} onClick={() => props.openPopup(index)}></button>
                    </li>
                  )
                )
              }
            </ul>
          </div>
        </div>
      </>
    );
  };


export default LoadClients;