import { useContext, useState } from 'react';
import { LoadClientContext } from '../../contexts';


import "./styles.css"

const LoadClients = props => {
  const SORT_TYPES = {
    id: 'id',
    dateCreated: 'dateCreated',
  }

  const { loadClients } = useContext(LoadClientContext);

  const [excuteClients, setExecuteClients] = useState({});
  const [sortType, setSortType] = useState(SORT_TYPES.id);

  function handleToggleSelected(id) {
    setExecuteClients({
      ...excuteClients,
      [id]: !excuteClients[id],
    })
  }

  const sortByProperty = type => (a,b) => {
    const sortProperty = SORT_TYPES[type];
    return a[sortProperty] - b[sortProperty];
  };

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
              <option value={SORT_TYPES.id}>Name</option>
              <option value={SORT_TYPES.dateCreated}>Date Created</option>
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
                loadClients.sort(sortByProperty(sortType)).map((lc, index) => (
                    <li key={lc.id}>
                      <button className={`loadclient ${excuteClients[lc.id] ? "selected" : ""}`} onClick={() => handleToggleSelected(lc.id)}>{lc.name}</button>
                      <div className="divider"></div> 
                      <button className={`load-client-status ${lc.currstatus}`} title={lc.currstatus} onClick={() => props.handleOpen(index)}></button>
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