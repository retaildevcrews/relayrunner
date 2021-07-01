import React from "react";

import "./styles.css"
 
const LoadClientDetails = props => {
  console.log(props.clientDetails[props.clientIndex])
  return (
    <div className="main">
      <div className="popup-box">
        <div className="box">
          <span className="close-icon" onClick={props.closePopup}>x</span>
          <h1>{props.clientDetails[props.clientIndex].name}</h1>
          <p>version: {props.clientDetails[props.clientIndex].version}</p>
          <p>ID: {props.clientDetails[props.clientIndex].id}</p>
          <p>Region: {props.clientDetails[props.clientIndex].region}</p>
          <p>Zone: {props.clientDetails[props.clientIndex].zone}</p>
          <p>Scheduler: {props.clientDetails[props.clientIndex].scheduler}</p>
          <p>Status: {props.clientDetails[props.clientIndex].currstatus}</p>
          <p>Metrics: {props.clientDetails[props.clientIndex].metrics}</p>
        </div>
      </div>
    </div>
  );
};
 
export default LoadClientDetails;