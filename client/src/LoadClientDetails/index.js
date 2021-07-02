import React, { useContext } from "react";
import LoadClientContext from "../LoadClientContext";

import "./styles.css"
 
const LoadClientDetails = props => {
  const { loadclients, currClientDetails, handleClose } = useContext(LoadClientContext);

  return (
    <div className="main">
      <div className="popup-box">
        <div className="box">
          <span className="close-icon" onClick={handleClose}>x</span>
          <h1>{loadclients[currClientDetails].name}</h1>
          <p>version: {loadclients[currClientDetails].version}</p>
          <p>ID: {loadclients[currClientDetails].id}</p>
          <p>Region: {loadclients[currClientDetails].region}</p>
          <p>Zone: {loadclients[currClientDetails].zone}</p>
          <p>Scheduler: {loadclients[currClientDetails].scheduler}</p>
          <p>Status: {loadclients[currClientDetails].currstatus}</p>
          <p>Metrics: {loadclients[currClientDetails].metrics}</p>
        </div>
      </div>
    </div>
  );
};
 
export default LoadClientDetails;
