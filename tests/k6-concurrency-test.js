import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  scenarios: {
    ticket_reservation_burst: {
      executor: 'per-vu-iterations',
      vus: 100, // 100 Virtual Users
      iterations: 1, // Each VU sends 1 reservation attempt simultaneously
      maxDuration: '30s',
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.01'], // <1% network failures allowed
    http_req_duration: ['p(95)<500'], // 95% requests completed under 500ms
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

export default function () {
  const payload = JSON.stringify({
    eventId: 1,
    ticketTypeId: 10,
    quantity: 1,
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${__ENV.JWT_TOKEN || 'test-token'}`,
    },
  };

  const res = http.post(`${BASE_URL}/api/Events/tickets/10/reserve`, payload, params);

  check(res, {
    'Status is 200 or 400 (Sold out)': (r) => r.status === 200 || r.status === 400,
    'Response contains reserved status': (r) => {
      if (r.status === 200) {
        const body = JSON.parse(r.body);
        return body.success === true;
      }
      return true; // 400 when sold out is acceptable
    },
  });

  sleep(0.1);
}
