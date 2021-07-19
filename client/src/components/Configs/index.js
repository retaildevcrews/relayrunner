import { useContext, useState } from 'react';
import ConfigsContext from '../ConfigsContext';

import "./styles.css"

const Configs = () => {
    const { configs, loadtests } = useContext(ConfigsContext);

    const [currConfig, setCurrConfig] = useState(-1);
    const [currLoadTests, setCurrLoadTests] = useState([]);
    const [selectedLoadTest, setSetlectedLoadTest] = useState(-1);

    const config = configs.find((c) => c.id === currConfig);
    // const loadTestPath = loadtests.find((c) => c.id === selectedLoadTest);

    const currentConfig = (id) => {
        if (currConfig === id) {
            setCurrConfig(-1);
        }
        else {
            setCurrConfig(id);
        }
    }

    const currentLoadTests = () => {
        if (currConfig > 0) {
            for (let i in loadtests) {
                if (loadtests[i].config === currConfig) {
                    setCurrLoadTests(loadtest => {
                        return [
                            ...loadtest, 
                            loadtests[i]
                        ]
                    });
                    continue;
                }
            };
        }
        else {
            setCurrLoadTests([]);
        }
    }
        
    const configSelect = (id) => {
        currentConfig(id);
        console.log(currConfig);
        currentLoadTests();
    }
    // console.log(currLoadTests)
    // console.log(currConfig)


    const loadTestSelect = (id) => {
        if (selectedLoadTest === id) {
            setSetlectedLoadTest(-1);
        }
        else {
            setSetlectedLoadTest(id);
        }
    }
    return (
        <div className="main">
            <div id="configpath">
                {config && <p><b>{config.name} </b></p>}
                {/* {loadTestPath && <p> > {loadTestPath.name}</p>} */}
            </div>
            <hr className="horizontal1"></hr>
            <hr className="horizontal2"></hr>
            <div className="configs">
                <h1>Configs</h1>
                <div>
                    <ul>
                    {
                        configs.map((c) => (
                            <li key={c.id}>
                                <button className={`configslist ${c.id === currConfig ? "selected" : ""}`} onClick={() => {configSelect(c.id)}}>{c.name}</button>
                            </li>
                        )
                        )
                    }
                    </ul>
                </div>
            </div>
            <hr className="vertical"></hr>
            <div className="loadtests">
                {currConfig > 0 && 
                <h1>LoadTests</h1>}
                <div>
                    <ul>
                    {
                        currLoadTests.map((lt) => (
                            <li key={lt.id}>
                                <button className={`configslist ${lt.id === selectedLoadTest ? "selected" : ""}`} onClick={() => {loadTestSelect(lt.id)}}>{lt.name}</button>
                            </li>
                        )
                        )
                    }
                    </ul>
                </div>
            </div>           
        </div>

    )
}

export default Configs;