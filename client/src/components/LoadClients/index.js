import { useContext, useState, useEffect } from 'react';
import LoadClientContext from '../LoadClientContext';


import "./styles.css"

const LoadClients = props => {
  const { loadClients} = useContext(LoadClientContext);
  const [excuteClients, setExecuteClients] = useState({});

  function handleToggleSelected(id) {
    setExecuteClients({
      ...excuteClients,
      [id]: !excuteClients[id],
    })
  }

  const [sortData, setSortData] = useState([]);
  const [sortType, setSortType] = useState('name');

  useEffect(() => {
    const sortArray = type => {
      const types = {
        id: 'id',
        dateCreated: 'dateCreated',
      };
      const sortProperty = types[type];
      const sorted = [...loadClients].sort((a, b) => a[sortProperty] - b[sortProperty]);
      setSortData(sorted);
    };

    sortArray(sortType);
  }, [sortType, loadClients]); 


    return (
      <>
        <div className="sidenav">
          <div className="header">
          <div>
              <h1>Load Clients</h1>
          </div>
          <div>
            <select onChange={(e) => setSortType(e.target.value)}>
              <option value="sort">Sort By:</option>
              <option value="id">Name</option>
              <option value="dateCreated">Date Created</option>
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
                sortData.map((lc) => (
                    <li key={lc.id}>
                      <button className={`loadclient ${excuteClients[lc.id] ? "selected" : ""}`} onClick={() => handleToggleSelected(lc.id)}>{lc.name}</button>
                      <div className="divider"></div> 
                      <button className={`load-client-status ${lc.currstatus}`} title={lc.currstatus} onClick={() => props.handleOpen(lc.id)}></button>
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