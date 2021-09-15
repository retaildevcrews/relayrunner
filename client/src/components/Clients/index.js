import { useContext, useState } from "react";
import PropTypes from "prop-types";
import { ClientContext } from "../../contexts";
import { CLIENT, CLIENT_STATUSES } from "../../models";
import "./styles.css";

const SORT_TYPES = {
  name: {
    key: CLIENT.name,
    label: "Name",
  },
  dateCreated: {
    key: CLIENT.startTime,
    label: "Date Created",
  },
};

const STATUSES = [
  {
    key: CLIENT_STATUSES.starting,
    label: "Starting",
  },
  {
    key: CLIENT_STATUSES.ready,
    label: "Ready",
  },
  {
    key: CLIENT_STATUSES.testing,
    label: "Busy",
  },
  {
    key: CLIENT_STATUSES.terminating,
    label: "Shutting Down",
  },
];

const sortByProperty = (type) => (a, b) => {
  const sortProperty = SORT_TYPES[type].key;
  return a[sortProperty] - b[sortProperty];
};

const Clients = ({ openClientDetails }) => {
  const { clients } = useContext(ClientContext);
  const [selectedClients, setSelectedClients] = useState({});
  const [filterBy, setFilterBy] = useState("");
  const [sortBy, setSortBy] = useState(SORT_TYPES.dateCreated.key);

  function toggleClient(loadClientId) {
    setSelectedClients({
      ...selectedClients,
      [loadClientId]: !selectedClients[loadClientId],
    });
  }

  return (
    <>
      <h1>Clients</h1>
      <select
        selected={sortBy}
        onChange={({ target }) => setSortBy(target.value)}
      >
        <option value={SORT_TYPES.name.key}>{SORT_TYPES.name.label}</option>
        <option value={SORT_TYPES.dateCreated.key}>
          {SORT_TYPES.dateCreated.label}
        </option>
      </select>
      <select onChange={({ target }) => setFilterBy(target.value)}>
        <option value="">Filter By:</option>
        {STATUSES.map(({ key, label }) => (
          <option key={key} value={key}>
            {label}
          </option>
        ))}
      </select>
      <ul>
        {clients
          .filter(
            ({ [CLIENT.status]: status }) =>
              filterBy === "" || filterBy === status
          )
          .sort(sortByProperty(sortBy))
          .map((c, index) => {
            const {
              [CLIENT.loadClientId]: loadClientId,
              [CLIENT.status]: status,
              [CLIENT.name]: name,
            } = c;

            return (
              <li key={loadClientId}>
                <button
                  type="button"
                  onClick={() => toggleClient(loadClientId)}
                >
                  {name}
                </button>
                <button
                  type="button"
                  title={status}
                  aria-label={status}
                  onClick={() => openClientDetails(index)}
                >
                  Open Details
                </button>
              </li>
            );
          })}
      </ul>
    </>
  );
};

Clients.propTypes = {
  openClientDetails: PropTypes.func.isRequired,
};

export { Clients as default, sortByProperty };
