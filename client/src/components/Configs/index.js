import { useContext, useState } from "react";
import { ConfigsContext } from "../../contexts";

import "./styles.css";

const Configs = () => {
  const { configs, loadTests } = useContext(ConfigsContext);

  const [currConfigId, setcurrConfigId] = useState(-1);
  const [selectedLoadTestId, setselectedLoadTestId] = useState(-1);

  const configSelect = (id) => () => {
    if (currConfigId === id) {
      setcurrConfigId(-1);
    }
    else {
      setcurrConfigId(id);
    }
    loadTestSelect(-1);
  };

  const loadTestSelect = (id) => (selectedLoadTestId === id ? setselectedLoadTestId(-1) : setselectedLoadTestId(id));

  const config = configs.find((c) => c.id === currConfigId);
  const loadTestPath = loadTests.find((c) => c.id === selectedLoadTestId);

  console.log(loadTestPath);

  return (
    <div className="main">
      <div id="configpath">
        {config && (
          <p>
            <b>{config.name} &nbsp;</b>
          </p>
        )}
        {loadTestPath && config.id === loadTestPath.configId && (
          <p>
            <b>&gt; {loadTestPath.name}</b>
          </p>
        )}
      </div>
      <hr className="horizontal1" />
      <hr className="horizontal2" />
      <div className="configs">
        <h1>Configs</h1>
        <div>
          <ul>
            {configs.map((c) => (
              <li key={c.id}>
                <button
                  type="button"
                  className={`configslist ${
                    c.id === currConfigId ? "selected" : ""
                  }`}
                  onClick={configSelect(c.id)}
                >
                  {c.name}
                </button>
              </li>
            ))}
          </ul>
        </div>
      </div>
      <hr className="vertical" />
      <div className="loadtests">
        {currConfigId > 0 && <h1>Load Tests</h1>}
        <div>
          <ul>
            {loadTests
              .filter((lt) => lt.configId === currConfigId)
              .map((lt) => (
                <li key={lt.id}>
                  <button
                    type="button"
                    className={`configslist ${
                      lt.id === selectedLoadTestId ? "selected" : ""
                    }`}
                    onClick={() => {
                      loadTestSelect(lt.id);
                    }}
                  >
                    {lt.name}
                  </button>
                </li>
              ))}
          </ul>
        </div>
      </div>
    </div>
  );
};

export default Configs;
