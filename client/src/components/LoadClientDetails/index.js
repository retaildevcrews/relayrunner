import React, { useContext } from "react";
import LoadClientContext from "../LoadClientContext";

import "./styles.css"
 
const LoadClientDetails = props => {
  const { loadClients } = useContext(LoadClientContext);
  const details = loadClients[props.currClientDetails];

  return (
    <div className="main">
      <div className="popup-box">
        <div className="box">
          <span className="close-icon" onClick={props.handleClose}>x</span>
          <h1>{details.name}</h1>
          <p>version: {details.version}</p>
          <p>ID: {details.id}</p>
          <p>Region: {details.region}</p>
          <p>Zone: {details.zone}</p>
          <p>Scheduler: {details.scheduler}</p>
          <p>Status: {details.currstatus}</p>
          <p>Metrics: {details.metrics}</p>
        </div>
      </div>
    </div>
  );
};
 
export default LoadClientDetails;
